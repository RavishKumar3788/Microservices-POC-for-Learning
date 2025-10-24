// Generic HTTP Service using fetch API

const request = async <T, U>(
  endpoint: string,
  method = "GET",
  data: U | null = null,
  headers = {}
): Promise<T> => {
  const config = {
    method,
    headers: {
      "Content-Type": "application/json",
      ...headers,
    },
    body: null as string | null,
  };

  if (data) {
    config.body = JSON.stringify(data);
  }

  const response = await fetch(`${endpoint}`, config);
  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || "HTTP error");
  }
  return response.json() as Promise<T>;
};

const getRequest = async <T,>(endpoint: string): Promise<T> => {
  try {
    const config = {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    };

    const response = await fetch(`${endpoint}`, config);
    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || "HTTP error");
    }
    return response.json() as Promise<T>;
  } catch (error) {
    throw error;
  }
};

export const httpService = {
  get: <T,>(endpoint: string) => getRequest<T>(endpoint),
  post: (endpoint: string, data: any, headers = {}) =>
    request(endpoint, "POST", data, headers),
  put: (endpoint: string, data: any, headers = {}) =>
    request(endpoint, "PUT", data, headers),
  delete: (endpoint: string, headers = {}) =>
    request(endpoint, "DELETE", null, headers),
};
