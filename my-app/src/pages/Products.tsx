import { Container, Typography } from "@mui/material";

const Products = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Products
      </Typography>
      <Typography variant="body1">
        Here you can view all our products.
      </Typography>
    </Container>
  );
};

export default Products;
