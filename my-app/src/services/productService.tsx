import { httpService } from "../httpService/httpService";

const BASE_URL = process.env.NODE_ENV === 'development'
  ? 'http://localhost:8080/api/products'
  : '/api/products';

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
}

// Create a function to get products array from the httpService get method
const getProducts = async (): Promise<Product[]> => {
  const response = await httpService.get<Product[]>(`${BASE_URL}`);
  return response;
};

const addProducts = async (): Promise<Product[]> => {
  const response = await httpService.get<Product[]>(
    `${BASE_URL}/addProducts`
  );
  return response;
};

export const productService = {
  getProducts,
  addProducts,
};
