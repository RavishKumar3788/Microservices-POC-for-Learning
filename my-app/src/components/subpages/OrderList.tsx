import React, { useEffect, useState } from 'react';
import { orderService, OrderWithDetails } from '../../services/orderService';
import { Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';


const OrderList: React.FC = () => {
    const [orders, setOrders] = useState<OrderWithDetails[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetchOrders();
    }, []);

    const fetchOrders = async () => {
        try {
            setLoading(true);
            const data = await orderService.getOrderWithDetails();
            setOrders(data);
            setError(null);
        } catch (err) {
            setError('Failed to fetch orders');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading orders...</div>;
    if (error) return <div>Error: {error}</div>;

    return (
        <TableContainer component={Paper} sx={{ mt: 4, boxShadow: 3 }}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell sx={{ fontWeight: "bold" }}>User Name</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Product Name</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Country</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Quantity</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Total Price</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Order Date</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {orders.map((order) => (
                        <TableRow key={order.id} hover>
                            <TableCell>{order.userName}</TableCell>
                            <TableCell>{order.productName}</TableCell>
                            <TableCell>{order.country}</TableCell>
                            <TableCell>{order.quantity}</TableCell>
                            <TableCell>$ {order.totalPrice}</TableCell>
                            <TableCell>{new Date(order.createdAt).toLocaleDateString()}</TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
};

export default OrderList;