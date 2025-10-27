import { Container, Typography } from "@mui/material";
import ProductList from "../components/subpages/ProductList";

const Products = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Products
      </Typography>
      <ProductList />
    </Container>
  );
};

export default Products;
