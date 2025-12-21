import { Container, Typography } from "@mui/material";
import OrderList from "../components/subpages/OrderList";

const Orders = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Orders
      </Typography>
      <OrderList />
    </Container>
  );
};

export default Orders;
