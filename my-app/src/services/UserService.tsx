import { httpService } from "../httpService/httpService";

const BASE_URL = '/api/users';

export interface User {
  id: string;
  name: string;
  email: string;
  country: string;
}

// Create a function to get products array from the httpService get method
const getUsers = async (): Promise<User[]> => {
  const response = await httpService.get<User[]>(`${BASE_URL}`);
  return response;
};

const addUsers = async (): Promise<User[]> => {
  const response = await httpService.get<User[]>(`${BASE_URL}/addUsers`);
  return response;
};

export const userService = {
  getUsers,
  addUsers,
};
