import { useCallback, useEffect, useState } from "react";
import {
    Container,
    Typography,
    Card,
    CardContent,
    Box,
    CircularProgress,
} from "@mui/material";
import {
    ComposableMap,
    Geographies,
    Geography,
    Marker,
} from "react-simple-maps";
import { orderService, OrderWithDetails } from "../services/orderService";
import { countryCoordinates } from "../constants/countryCoordinates";
import PublicIcon from "@mui/icons-material/Public";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";

interface CountryData {
    country: string;
    orderCount: number;
    totalRevenue: number;
    coordinates: [number, number];
}

interface CountryDataMap {
    country: string;
    orderCount: number;
    totalRevenue: number;
}

const geoUrl = "https://raw.githubusercontent.com/datasets/geo-countries/master/data/countries.geojson";

const Dashboard = () => {
    const [loading, setLoading] = useState(true);
    const [countryData, setCountryData] = useState<CountryData[]>([]);
    const [totalOrders, setTotalOrders] = useState(0);
    const [totalRevenue, setTotalRevenue] = useState(0);
    const [maxOrders, setMaxOrders] = useState(0);

    const fetchOrderData = useCallback(async () => {
        try {
            setLoading(true);
            const orders: OrderWithDetails[] = await orderService.getOrderWithDetails();
            processOrders(orders);
        } catch (error) {
            console.error("Error fetching order data:", error);
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        let cleanup: (() => void) | undefined;

        const setupStream = async () => {
            // Initial fetch
            await fetchOrderData();

            // Setup real-time streaming
            cleanup = await orderService.streamOrdersWithDetails(
                (orders) => {
                    processOrders(orders);
                },
                (error) => {
                    console.error('Stream error:', error);
                }
            );
        };

        setupStream();

        // Cleanup on unmount
        return () => {
            if (cleanup) {
                cleanup();
            }
        };
    }, [fetchOrderData]);

    const processOrders = (orders: OrderWithDetails[]) => {
        // Group orders by country
        const countryMap = new Map<string, CountryDataMap>();

        orders.forEach((order) => {
            const country = order.country || "Unknown";
            if (countryMap.has(country)) {
                const existing = countryMap.get(country)!;
                existing.orderCount += 1;
                existing.totalRevenue += order.totalPrice;
            } else {
                countryMap.set(country, {
                    country,
                    orderCount: 1,
                    totalRevenue: order.totalPrice,
                });
            }
        });

        // Add coordinates and filter out countries without coordinates
        const dataWithCoords: CountryData[] = Array.from(countryMap.values())
            .filter((data: CountryDataMap) => countryCoordinates[data.country])
            .map((data: CountryDataMap): CountryData => ({
                country: data.country,
                orderCount: data.orderCount,
                totalRevenue: data.totalRevenue,
                coordinates: countryCoordinates[data.country],
            }))
            .sort((a: CountryData, b: CountryData) => b.orderCount - a.orderCount);

        const maxOrderCount = Math.max(...dataWithCoords.map(d => d.orderCount), 1);

        setCountryData(dataWithCoords);
        setTotalOrders(orders.length);
        setTotalRevenue(
            orders.reduce((sum, order) => sum + order.totalPrice, 0)
        );
        setMaxOrders(maxOrderCount);
        setLoading(false);
    };



    const getMarkerSize = (orderCount: number) => {
        // Scale marker size based on order count (min 4, max 30)
        const minSize = 4;
        const maxSize = 30;
        return minSize + (orderCount / maxOrders) * (maxSize - minSize);
    };

    if (loading) {
        return (
            <Container className="mt-8 flex justify-center items-center h-96">
                <CircularProgress />
            </Container>
        );
    }

    return (
        <Container maxWidth="xl" className="mt-8">
            {/* Summary Cards */}
            <Box className="mb-6" sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
                <Box sx={{ flex: '1 1 300px', minWidth: 0 }}>
                    <Card elevation={3}>
                        <CardContent>
                            <Box className="flex items-center justify-between">
                                <Box>
                                    <Typography color="text.secondary" gutterBottom>
                                        Total Orders
                                    </Typography>
                                    <Typography variant="h4">{totalOrders}</Typography>
                                </Box>
                                <ShoppingCartIcon
                                    sx={{ fontSize: 48, color: "#0088FE", opacity: 0.5 }}
                                />
                            </Box>
                        </CardContent>
                    </Card>
                </Box>
                <Box sx={{ flex: '1 1 300px', minWidth: 0 }}>
                    <Card elevation={3}>
                        <CardContent>
                            <Box className="flex items-center justify-between">
                                <Box>
                                    <Typography color="text.secondary" gutterBottom>
                                        Countries
                                    </Typography>
                                    <Typography variant="h4">{countryData.length}</Typography>
                                </Box>
                                <PublicIcon
                                    sx={{ fontSize: 48, color: "#00C49F", opacity: 0.5 }}
                                />
                            </Box>
                        </CardContent>
                    </Card>
                </Box>
                <Box sx={{ flex: '1 1 300px', minWidth: 0 }}>
                    <Card elevation={3}>
                        <CardContent>
                            <Box>
                                <Typography color="text.secondary" gutterBottom>
                                    Total Revenue
                                </Typography>
                                <Typography variant="h4">
                                    ${totalRevenue.toFixed(2)}
                                </Typography>
                            </Box>
                        </CardContent>
                    </Card>
                </Box>
            </Box>

            {/* Proportional Symbol Map */}
            <Card elevation={3}>
                <CardContent>
                    <Box sx={{ width: '100%', height: '600px', position: 'relative' }}>
                        <ComposableMap
                            projection="geoMercator"
                            projectionConfig={{
                                scale: 147,
                            }}
                            style={{ width: '100%', height: '100%' }}
                        >
                            <Geographies geography={geoUrl}>
                                {({ geographies }: { geographies: any[] }) =>
                                    geographies.map((geo: any) => (
                                        <Geography
                                            key={geo.rsmKey}
                                            geography={geo}
                                            fill="#DDD"
                                            stroke="#FFF"
                                            strokeWidth={0.5}
                                            style={{
                                                default: { outline: 'none' },
                                                hover: { fill: '#CCC', outline: 'none' },
                                                pressed: { outline: 'none' },
                                            }}
                                        />
                                    ))
                                }
                            </Geographies>
                            {countryData.map((country, index) => (
                                <Marker key={index} coordinates={country.coordinates}>
                                    <circle
                                        r={getMarkerSize(country.orderCount)}
                                        fill="#0088FE"
                                        fillOpacity={0.6}
                                        stroke="#fff"
                                        strokeWidth={1}
                                    />
                                    <title>{`${country.country}: ${country.orderCount} orders`}</title>
                                </Marker>
                            ))}
                        </ComposableMap>
                    </Box>

                    {/* Legend */}
                    <Box sx={{ mt: 2, display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 2 }}>
                        <Typography variant="body2" color="text.secondary">
                            Circle size represents number of orders
                        </Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <circle
                                style={{
                                    width: '8px',
                                    height: '8px',
                                    borderRadius: '50%',
                                    backgroundColor: '#0088FE',
                                    display: 'inline-block',
                                }}
                            />
                            <Typography variant="caption">Few</Typography>
                            <circle
                                style={{
                                    width: '20px',
                                    height: '20px',
                                    borderRadius: '50%',
                                    backgroundColor: '#0088FE',
                                    display: 'inline-block',
                                    marginLeft: '8px',
                                }}
                            />
                            <Typography variant="caption">Many</Typography>
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Top Countries Table */}
            {/* <Box sx={{ mt: 3 }}>
                <Card elevation={3}>
                    <CardContent>
                        <Typography variant="h6" gutterBottom>
                            Top Countries by Orders
                        </Typography>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                            {countryData.slice(0, 10).map((country, index) => (
                                <Box
                                    key={index}
                                    sx={{
                                        display: 'flex',
                                        justifyContent: 'space-between',
                                        p: 1.5,
                                        bgcolor: index % 2 === 0 ? '#f5f5f5' : 'transparent',
                                        borderRadius: 1,
                                    }}
                                >
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                        <Typography variant="body1" fontWeight="bold" color="text.secondary">
                                            #{index + 1}
                                        </Typography>
                                        <Typography variant="body1">{country.country}</Typography>
                                    </Box>
                                    <Box sx={{ display: 'flex', gap: 4 }}>
                                        <Box sx={{ textAlign: 'right' }}>
                                            <Typography variant="body2" color="text.secondary">
                                                Orders
                                            </Typography>
                                            <Typography variant="body1" fontWeight="bold">
                                                {country.orderCount}
                                            </Typography>
                                        </Box>
                                        <Box sx={{ textAlign: 'right' }}>
                                            <Typography variant="body2" color="text.secondary">
                                                Revenue
                                            </Typography>
                                            <Typography variant="body1" fontWeight="bold">
                                                ${country.totalRevenue.toFixed(2)}
                                            </Typography>
                                        </Box>
                                    </Box>
                                </Box>
                            ))}
                        </Box>
                    </CardContent>
                </Card>
            </Box> */}
        </Container>
    );
};

export default Dashboard;
