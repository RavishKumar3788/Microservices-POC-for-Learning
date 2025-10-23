import { Container, Typography } from "@mui/material";

const Orders = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Orders
      </Typography>
      <Typography variant="body1">
        View and manage your orders here...
      </Typography>
    </Container>
  );
};

export default Orders;
