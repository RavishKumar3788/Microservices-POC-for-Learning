import { httpService } from "../httpService/httpService";

const BASE_URL = process.env.REACT_APP_USER_API_BASE_URL || "";

export interface User {
  id: string;
  name: string;
  email: string;
  country: string;
}

// Create a function to get products array from the httpService get method
const getUsers = async (): Promise<User[]> => {
  const response = await httpService.get<User[]>(`${BASE_URL}/Users`);
  return response;
};

const addUsers = async (): Promise<User[]> => {
  const response = await httpService.get<User[]>(`${BASE_URL}/users/addusers`);
  return response;
};

export const userService = {
  getUsers,
  addUsers,
};
