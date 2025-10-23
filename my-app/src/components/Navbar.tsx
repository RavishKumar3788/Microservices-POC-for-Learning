import { AppBar, Toolbar, Typography, Button } from "@mui/material";
import { useNavigate, useLocation } from "react-router-dom";
import { useTheme } from "../context/ThemeContext";
import HomeIcon from "@mui/icons-material/Home";
import ShoppingBasketIcon from "@mui/icons-material/ShoppingBasket";
import ListAltIcon from "@mui/icons-material/ListAlt";
import LightModeIcon from "@mui/icons-material/LightMode";
import DarkModeIcon from "@mui/icons-material/DarkMode";

const Navbar = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { darkMode, toggleDarkMode } = useTheme();

  const getButtonClass = (path: string) => {
    const isActive = location.pathname === path;
    return `hover:bg-blue-700 relative ${
      isActive
        ? "after:absolute after:bottom-0 after:left-0 after:w-full after:h-0.5 after:bg-white"
        : ""
    }`;
  };

  return (
    <AppBar position="static" className="bg-blue-600">
      <Toolbar className="justify-between">
        <Typography variant="h6" component="div" className="flex items-center">
          POC
        </Typography>
        <div className="flex gap-4">
          <Button
            color="inherit"
            startIcon={<HomeIcon />}
            onClick={() => navigate("/")}
            className={getButtonClass("/")}
          >
            Home
          </Button>
          <Button
            color="inherit"
            startIcon={<ShoppingBasketIcon />}
            onClick={() => navigate("/products")}
            className={getButtonClass("/products")}
          >
            Products
          </Button>
          <Button
            color="inherit"
            startIcon={<ListAltIcon />}
            onClick={() => navigate("/orders")}
            className={getButtonClass("/orders")}
          >
            Orders
          </Button>
          <Button
            variant="contained"
            onClick={toggleDarkMode}
            startIcon={darkMode ? <LightModeIcon /> : <DarkModeIcon />}
          >
            {darkMode ? "Light Mode" : "Dark Mode"}
          </Button>
        </div>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;
