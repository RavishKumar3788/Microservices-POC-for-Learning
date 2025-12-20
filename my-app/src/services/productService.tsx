import { httpService } from "../httpService/httpService";

const BASE_URL = '/api/products';

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
