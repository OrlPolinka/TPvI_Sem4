function Sum(a, b) {
    return a + b;
}

function Sub(a, b) {
    return a - b;
}

function Mul(a, b) {
    return a * b;
}

function Div(a, b) {
    if (b === 0) {
        return "Error: Division by zero";
    }
    return a / b;
}
