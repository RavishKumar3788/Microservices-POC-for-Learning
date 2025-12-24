import { httpService } from "../httpService/httpService";
import { productService } from "./productService";
import { userService } from "./UserService";

const BASE_URL = process.env.NODE_ENV === 'development'
    ? 'http://localhost:8080/api/orders'
    : '/api/orders';

export interface Order {
    id: string;
    userId: string;
    productId: string;
    productPrice: number;
    quantity: number;
    totalPrice: number;
    createdAt: Date;
    updatedAt: Date;
}

export interface OrderViewModel {
    userId: string;
    productId: string;
    productPrice: number;
    quantity: number;
}

export interface OrderWithDetails extends Order {
    productName: string;
    userName: string;
    country: string;
}

// Create a function to get orders array from the httpService get method
const getOrders = async (): Promise<Order[]> => {
    const response = await httpService.get<Order[]>(`${BASE_URL}`);
    return response;
};

const placeOrder = async (order: OrderViewModel): Promise<Order> => {
    const response = await httpService.post<Order, OrderViewModel>(`${BASE_URL}`, order);
    return response;
}

const getOrderWithDetails = async (): Promise<OrderWithDetails[]> => {
    const [orders, products, users] = await Promise.all([
        getOrders(),
        productService.getProducts(),
        userService.getUsers(),
    ]);

    const enhancedOrders = orders.map(order => {
        const product = products.find(p => p.id === order.productId);
        const user = users.find(u => u.id === order.userId);
        return {
            ...order,
            productName: product ? product.name : 'Unknown Product',
            userName: user ? user.name : 'Unknown User',
            country: user ? user.country : 'Unknown Country',
        };
    });


    return enhancedOrders;
}

const streamOrders = (
    onOrdersReceived: (orders: Order[]) => void,
    onError?: (error: Event) => void
): (() => void) => {
    const streamUrl = `${BASE_URL}/stream`;

    const eventSource = new EventSource(streamUrl);

    eventSource.onmessage = (event) => {
        try {
            const orders = JSON.parse(event.data);
            console.log('Received orders:', orders);
            onOrdersReceived(orders);
        } catch (error) {
            console.error('Error parsing orders:', error);
        }
    };

    eventSource.onerror = (error) => {
        console.error('EventSource error:', error);
        if (onError) {
            onError(error);
        }
    };

    // Return cleanup function to close the connection
    return () => {
        eventSource.close();
    };
};

const streamOrdersWithDetails = async (
    onOrdersReceived: (orders: OrderWithDetails[]) => void,
    onError?: (error: Event) => void
): Promise<(() => void)> => {
    // Fetch products and users once before starting the stream
    const [products, users] = await Promise.all([
        productService.getProducts(),
        userService.getUsers(),
    ]);

    return streamOrders((orders) => {
        try {
            const enhancedOrders = orders.map(order => {
                const product = products.find(p => p.id === order.productId);
                const user = users.find(u => u.id === order.userId);
                return {
                    ...order,
                    productName: product ? product.name : 'Unknown Product',
                    userName: user ? user.name : 'Unknown User',
                    country: user ? user.country : 'Unknown Country',
                };
            });

            onOrdersReceived(enhancedOrders);
        } catch (error) {
            console.error('Error enriching orders:', error);
        }
    }, onError);
};

export const orderService = {
    getOrders,
    placeOrder,
    getOrderWithDetails,
    streamOrders,
    streamOrdersWithDetails,
};
