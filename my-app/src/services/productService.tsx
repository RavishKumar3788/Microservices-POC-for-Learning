import { httpService } from "../httpService/httpService";

const BASE_URL = process.env.REACT_APP_PRODUCT_API_BASE_URL || "";

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
}

// Create a function to get products array from the httpService get method
const getProducts = async (): Promise<Product[]> => {
  const response = await httpService.get<Product[]>(`${BASE_URL}/products`);
  return response;
};

export const productService = {
  getProducts,
};
