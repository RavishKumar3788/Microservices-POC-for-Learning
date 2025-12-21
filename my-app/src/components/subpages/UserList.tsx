import React, { useEffect, useState, useCallback } from "react";
import { User, userService } from "../../services/UserService";
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
import AddIcon from "@mui/icons-material/Add";
import ArrowCircleRightTwoToneIcon from "@mui/icons-material/ArrowCircleRightTwoTone";
import GridShimmer from "./GridShimmer";
import ProductSelectionDialog from "./ProductSelectionDialog";

const UserList = () => {
  const [loading, setLoading] = useState(true);
  const [users, setUsers] = useState<User[]>([]);
  const [open, setOpen] = React.useState(false);
  const [selectedUserId, setSelectedUserId] = React.useState<string | null>(
    null
  );

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      const data = await userService.getUsers();
      setUsers(data);
    } catch (error) {
      // Optionally handle error state here
      console.error("Error fetching users:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const handleAddUser = useCallback(async () => {
    try {
      setLoading(true);
      const newUser = await userService.addUsers();
      setUsers((prevUsers) => [...prevUsers, ...newUser]);
    } catch (error) {
      // Optionally handle error state here
      console.error("Error adding user:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  const handleClickOpen = useCallback((userId: string) => {
    setSelectedUserId(userId);
    setOpen(true);
  }, []);

  const handleClose = useCallback(() => {
    setOpen(false);
  }, []);

  if (loading) return <GridShimmer />;

  if (users.length === 0) {
    return (
      <>
        <p>No users available.</p>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={handleAddUser}
        >
          Add Users
        </Button>
      </>
    );
  }

  return (
    <>
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
            {users.map((user) => (
              <TableRow key={user.id} hover>
                <TableCell>{user.name}</TableCell>
                <TableCell>{user.email}</TableCell>
                <TableCell>{user.country}</TableCell>
                <TableCell>
                  <Tooltip title="Select Product to Order">
                    <IconButton>
                      <ArrowCircleRightTwoToneIcon
                        color="primary"
                        onClick={() => handleClickOpen(user.id)}
                      />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <ProductSelectionDialog
        open={open}
        onClose={handleClose}
        userId={selectedUserId}
      />
    </>
  );
};

export default UserList;
