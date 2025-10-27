import { Container, Typography } from "@mui/material";
import UserList from "../components/subpages/UserList";

const Users = () => {
  return (
    <Container className="mt-8">
      <Typography variant="h4" component="h1" gutterBottom>
        Users
      </Typography>
      <Typography variant="body1">
        <UserList />
      </Typography>
    </Container>
  );
};

export default Users;
