import { useCallback, useState } from "react";
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Box,
  useMediaQuery,
  useTheme as useMuiTheme
} from "@mui/material";
import { useNavigate, useLocation } from "react-router-dom";
import { useTheme } from "../context/ThemeContext";
import HomeIcon from "@mui/icons-material/Home";
import ShoppingBasketIcon from "@mui/icons-material/ShoppingBasket";
import ListAltIcon from "@mui/icons-material/ListAlt";
import LightModeIcon from "@mui/icons-material/LightMode";
import DarkModeIcon from "@mui/icons-material/DarkMode";
import UserIcon from "@mui/icons-material/Person";
import MenuIcon from "@mui/icons-material/Menu";
import CloseIcon from "@mui/icons-material/Close";

const Navbar = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { darkMode, toggleDarkMode } = useTheme();
  const muiTheme = useMuiTheme();
  const isMobile = useMediaQuery(muiTheme.breakpoints.down('md'));
  const [drawerOpen, setDrawerOpen] = useState(false);

  const getButtonClass = useCallback((path: string) => {
    const isActive = location.pathname === path;
    return `hover:bg-blue-700 relative ${isActive
      ? "after:absolute after:bottom-0 after:left-0 after:w-full after:h-0.5 after:bg-white"
      : ""
      }`;
  }, [location.pathname]);

  const menuItems = [
    { path: "/", label: "Home", icon: <HomeIcon /> },
    { path: "/users", label: "Users", icon: <UserIcon /> },
    { path: "/products", label: "Products", icon: <ShoppingBasketIcon /> },
    { path: "/orders", label: "Orders", icon: <ListAltIcon /> },
  ];

  const handleNavigate = (path: string) => {
    navigate(path);
    setDrawerOpen(false);
  };

  const toggleDrawer = (open: boolean) => {
    setDrawerOpen(open);
  };

  return (
    <AppBar position="static" className="bg-blue-600">
      <Toolbar className="justify-between">
        <Typography variant="h6" component="div" className="flex items-center font-bold">
          POC
        </Typography>

        {/* Desktop Navigation */}
        {!isMobile && (
          <div className="flex gap-2">
            {menuItems.map((item) => (
              <Button
                key={item.path}
                color="inherit"
                startIcon={item.icon}
                onClick={() => navigate(item.path)}
                className={getButtonClass(item.path)}
              >
                {item.label}
              </Button>
            ))}
            <Button
              variant="contained"
              onClick={toggleDarkMode}
              startIcon={darkMode ? <LightModeIcon /> : <DarkModeIcon />}
              className="ml-2"
            >
              {darkMode ? "Light" : "Dark"}
            </Button>
          </div>
        )}

        {/* Mobile Menu Button */}
        {isMobile && (
          <Box className="flex items-center gap-2">
            <IconButton
              color="inherit"
              onClick={toggleDarkMode}
              aria-label="toggle theme"
            >
              {darkMode ? <LightModeIcon /> : <DarkModeIcon />}
            </IconButton>
            <IconButton
              color="inherit"
              edge="end"
              onClick={() => toggleDrawer(true)}
              aria-label="menu"
            >
              <MenuIcon />
            </IconButton>
          </Box>
        )}

        {/* Mobile Drawer */}
        <Drawer
          anchor="right"
          open={drawerOpen}
          onClose={() => toggleDrawer(false)}
        >
          <Box
            sx={{ width: 250 }}
            role="presentation"
          >
            <Box className="flex justify-between items-center p-4 bg-blue-600 text-white">
              <Typography variant="h6">Menu</Typography>
              <IconButton
                color="inherit"
                onClick={() => toggleDrawer(false)}
                aria-label="close menu"
              >
                <CloseIcon />
              </IconButton>
            </Box>
            <List>
              {menuItems.map((item) => (
                <ListItem key={item.path} disablePadding>
                  <ListItemButton
                    selected={location.pathname === item.path}
                    onClick={() => handleNavigate(item.path)}
                  >
                    <ListItemIcon>{item.icon}</ListItemIcon>
                    <ListItemText primary={item.label} />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>
          </Box>
        </Drawer>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;
