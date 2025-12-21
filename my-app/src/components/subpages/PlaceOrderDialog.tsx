import React, { useState, useCallback } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    IconButton,
    Slide,
    TextField,
    Typography,
    Box,
    Divider,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { TransitionProps } from "@mui/material/transitions";
import { Product } from "../../services/productService";
import { orderService, OrderViewModel } from "../../services/orderService";


const Transition = React.forwardRef(function Transition(
    props: TransitionProps & {
        children: React.ReactElement<any, any>;
    },
    ref: React.Ref<unknown>
) {
    return <Slide direction="up" ref={ref} {...props} />;
});

interface PlaceOrderDialogProps {
    open: boolean;
    onClose: () => void;
    product: Product | null;
    userId: string | null | undefined;
}

const PlaceOrderDialog: React.FC<PlaceOrderDialogProps> = ({
    open,
    onClose,
    product,
    userId,
}) => {
    const [quantity, setQuantity] = useState<number>(1);

    const handlePlaceOrder = useCallback(() => {
        if (!product || !userId) return;

        const orderDetails: OrderViewModel = {
            userId,
            productId: product.id,
            productPrice: product.price,
            quantity,
        };

        orderService.placeOrder(orderDetails).then((order) => {
            console.log("Order placed successfully:", order);
            // Reset form and close dialog
            setQuantity(1);
            onClose();
        }).catch((error) => {
            console.error("Error placing order:", error);
        });


    }, [product, userId, quantity, onClose]);

    const handleClose = useCallback(() => {
        setQuantity(1);
        onClose();
    }, [onClose]);

    if (!product) return null;

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            slots={{
                transition: Transition,
            }}
            fullWidth={true}
            maxWidth="sm"
        >
            <DialogTitle>
                Place Order
                <IconButton
                    aria-label="close"
                    onClick={handleClose}
                    sx={(theme) => ({
                        position: "absolute",
                        right: 8,
                        top: 8,
                        color: theme.palette.grey[500],
                    })}
                >
                    <CloseIcon />
                </IconButton>
            </DialogTitle>
            <DialogContent>
                <Box sx={{ mt: 2 }}>
                    <Typography variant="h6" gutterBottom>
                        {product.name}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                        {product.description}
                    </Typography>
                    <Typography variant="h6" color="primary" sx={{ mt: 2, mb: 2 }}>
                        ${product.price}
                    </Typography>
                    <Divider sx={{ mb: 3 }} />

                    <TextField
                        label="Quantity"
                        type="number"
                        fullWidth
                        value={quantity}
                        onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 1))}
                        inputProps={{ min: 1 }}
                        sx={{ mb: 2 }}
                    />

                    <Box
                        sx={{
                            mt: 3,
                            p: 2,
                            bgcolor: "grey.100",
                            borderRadius: 1,
                        }}
                    >
                        <Typography variant="subtitle1" gutterBottom>
                            Order Summary
                        </Typography>
                        <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
                            <Typography variant="body2">Quantity:</Typography>
                            <Typography variant="body2" fontWeight="bold">
                                {quantity}
                            </Typography>
                        </Box>
                        <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                            <Typography variant="body1" fontWeight="bold">
                                Total:
                            </Typography>
                            <Typography variant="body1" fontWeight="bold" color="primary">
                                ${(product.price * quantity).toFixed(2)}
                            </Typography>
                        </Box>
                    </Box>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} variant="outlined">
                    Cancel
                </Button>
                <Button onClick={handlePlaceOrder} variant="contained" color="primary">
                    Place Order
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default PlaceOrderDialog;
