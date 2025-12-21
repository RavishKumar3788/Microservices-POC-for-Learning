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

export const orderService = {
    getOrders,
    placeOrder,
    getOrderWithDetails,
};
