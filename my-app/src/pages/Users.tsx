import { Container, Typography } from "@mui/material";

const Users = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Users
      </Typography>
      <Typography variant="body1">
        View and manage your users here...
      </Typography>
    </Container>
  );
};

export default Users;
