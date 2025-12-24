import { useEffect, useState } from 'react';
import { Box } from '@mui/material';

interface AnimatedCounterProps {
    value: number;
    duration?: number;
    formatValue?: (value: number) => string;
}

export const AnimatedCounter: React.FC<AnimatedCounterProps> = ({
    value,
    duration = 1000,
    formatValue = (v) => v.toString(),
}) => {
    const [displayValue, setDisplayValue] = useState(value);
    const [isAnimating, setIsAnimating] = useState(false);

    useEffect(() => {
        const startValue = displayValue;
        const endValue = value;
        const startTime = Date.now();

        if (startValue === endValue) return;

        setIsAnimating(true);

        const animate = () => {
            const now = Date.now();
            const progress = Math.min((now - startTime) / duration, 1);

            // Easing function for smooth animation
            const easeOutQuart = 1 - Math.pow(1 - progress, 4);

            const currentValue = startValue + (endValue - startValue) * easeOutQuart;

            setDisplayValue(currentValue);

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                setDisplayValue(endValue);
                setIsAnimating(false);
            }
        };

        requestAnimationFrame(animate);
    }, [value, duration]);

    const formattedValue = formatValue(displayValue);

    return (
        <Box
            component="span"
            sx={{
                display: 'inline-block',
                position: 'relative',
                overflow: 'hidden',
            }}
        >
            {formattedValue.split('').map((char, index) => (
                <Box
                    key={`${char}-${index}`}
                    component="span"
                    sx={{
                        display: 'inline-block',
                        animation: isAnimating ? 'slideUp 0.5s ease-out' : 'none',
                        '@keyframes slideUp': {
                            '0%': {
                                transform: 'translateY(100%)',
                                opacity: 0,
                            },
                            '50%': {
                                opacity: 0.5,
                            },
                            '100%': {
                                transform: 'translateY(0)',
                                opacity: 1,
                            },
                        },
                    }}
                >
                    {char}
                </Box>
            ))}
        </Box>
    );
};
