import React, { useState } from "react";
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
    const [notes, setNotes] = useState<string>("");

    const handlePlaceOrder = () => {
        if (!product || !userId) return;

        // TODO: Implement order placement logic here
        console.log("Placing order:", {
            userId,
            productId: product.id,
            quantity,
            notes,
            totalPrice: product.price * quantity,
        });

        // Reset form and close dialog
        setQuantity(1);
        setNotes("");
        onClose();
    };

    const handleClose = () => {
        setQuantity(1);
        setNotes("");
        onClose();
    };

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

                    <TextField
                        label="Order Notes (Optional)"
                        multiline
                        rows={3}
                        fullWidth
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                        placeholder="Add any special instructions..."
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
