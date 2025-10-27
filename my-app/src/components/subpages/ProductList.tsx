import React, { useState, useEffect } from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  Tooltip,
  IconButton,
} from "@mui/material";
import { Product, productService } from "../../services/productService";
import AddIcon from "@mui/icons-material/Add";
import GridShimmer from "./GridShimmer";
import ShoppingCartTwoToneIcon from "@mui/icons-material/ShoppingCartTwoTone";

type ProductListProps = {
  userId?: string | null;
  asModel?: boolean;
};

const ProductList = ({ userId, asModel }: ProductListProps) => {
  console.log(userId);
  console.log(asModel);
  const [loading, setLoading] = useState(true);
  const [products, setProducts] = useState<Product[]>([]);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await productService.getProducts();
      setProducts(data);
    } catch (error) {
      // Optionally handle error state here
      console.error("Error fetching products:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleAddProduct = async () => {
    try {
      setLoading(true);
      const newProduct = await productService.addProducts();
      setProducts((prevProducts) => [...prevProducts, ...newProduct]);
    } catch (error) {
      // Optionally handle error state here
      console.error("Error adding product:", error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <GridShimmer />;

  if (products.length === 0) {
    return (
      <>
        <p>No products available.</p>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={handleAddProduct}
        >
          Add Products
        </Button>
      </>
    );
  }

  return (
    <TableContainer component={Paper} sx={{ mt: 4, boxShadow: 3 }}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: "bold" }}>Name</TableCell>
            <TableCell sx={{ fontWeight: "bold" }}>Description</TableCell>
            <TableCell sx={{ fontWeight: "bold" }}>Price</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {products.map((product) => (
            <TableRow key={product.id} hover>
              <TableCell>{product.name}</TableCell>
              <TableCell>{product.description}</TableCell>
              <TableCell>${product.price}</TableCell>
              <TableCell>
                <Tooltip title="Place Order">
                  <IconButton>
                    <ShoppingCartTwoToneIcon color="primary" />
                  </IconButton>
                </Tooltip>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default ProductList;
