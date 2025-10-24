import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import Home from "./pages/Home";
import Products from "./pages/Products";
import Orders from "./pages/Orders";
import "./App.css";
import { ThemeProvider as MuiThemeProvider } from "@emotion/react";
import { CssBaseline } from "@mui/material";
import { darkTheme, lightTheme } from "./components/theme";
import { ThemeProvider, useTheme } from "./context/ThemeContext";
import Users from "./pages/Users";

export function App() {
  return (
    <ThemeProvider>
      <AppContent />
    </ThemeProvider>
  );
}

function AppContent() {
  const { darkMode } = useTheme();

  return (
    <MuiThemeProvider theme={darkMode ? darkTheme : lightTheme}>
      <CssBaseline />
      <Router>
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
          <Navbar />
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/users" element={<Users />} />
            <Route path="/products" element={<Products />} />
            <Route path="/orders" element={<Orders />} />
          </Routes>
        </div>
      </Router>
    </MuiThemeProvider>
  );
}

export default App;
