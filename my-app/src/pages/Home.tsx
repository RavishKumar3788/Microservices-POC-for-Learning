import { Container, Typography } from "@mui/material";

const Home = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Welcome to Home Page
      </Typography>
      <Typography variant="body1">
        This is the home page of our application.
      </Typography>
    </Container>
  );
};

export default Home;
