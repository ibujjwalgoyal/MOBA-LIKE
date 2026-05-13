#pragma once

#include <cstdint>
#include <limits>
#include <algorithm>

namespace moba::math {

class FixedPoint {
public:
    static constexpr int32_t SCALE = 1000;
    int32_t raw_value;

    constexpr FixedPoint() : raw_value(0) {}
    explicit constexpr FixedPoint(int32_t v) : raw_value(v) {}

    // Create from integer (no scaling)
    static constexpr FixedPoint FromInt(int32_t value) {
        return FixedPoint(value * SCALE);
    }

    // Create from raw value (already scaled)
    static constexpr FixedPoint FromRaw(int32_t raw) {
        return FixedPoint(raw);
    }

    // Conversion to float (for debug only)
    constexpr float ToFloat() const {
        return static_cast<float>(raw_value) / SCALE;
    }

    // Operators
    constexpr FixedPoint operator+(const FixedPoint& other) const {
        return FixedPoint(raw_value + other.raw_value);
    }

    constexpr FixedPoint operator-(const FixedPoint& other) const {
        return FixedPoint(raw_value - other.raw_value);
    }

    constexpr FixedPoint operator-() const {
        return FixedPoint(-raw_value);
    }

    constexpr FixedPoint operator*(const FixedPoint& other) const {
        // Use 64-bit intermediate to avoid overflow
        return FixedPoint(static_cast<int32_t>((static_cast<int64_t>(raw_value) * other.raw_value) / SCALE));
    }

    constexpr FixedPoint operator/(const FixedPoint& other) const {
        // (a / b) scaled: multiply numerator by SCALE then divide by denominator
        return FixedPoint(static_cast<int32_t>((static_cast<int64_t>(raw_value) * SCALE) / other.raw_value));
    }

    constexpr FixedPoint& operator+=(const FixedPoint& other) {
        raw_value += other.raw_value;
        return *this;
    }

    constexpr FixedPoint& operator-=(const FixedPoint& other) {
        raw_value -= other.raw_value;
        return *this;
    }

    constexpr FixedPoint& operator*=(const FixedPoint& other) {
        raw_value = static_cast<int32_t>((static_cast<int64_t>(raw_value) * other.raw_value) / SCALE);
        return *this;
    }

    constexpr FixedPoint& operator/=(const FixedPoint& other) {
        raw_value = static_cast<int32_t>((static_cast<int64_t>(raw_value) * SCALE) / other.raw_value);
        return *this;
    }

    // Comparison operators
    friend constexpr bool operator==(const FixedPoint& lhs, const FixedPoint& rhs) {
        return lhs.raw_value == rhs.raw_value;
    }
    friend constexpr bool operator!=(const FixedPoint& lhs, const FixedPoint& rhs) {
        return lhs.raw_value != rhs.raw_value;
    }
    friend constexpr bool operator<(const FixedPoint& lhs, const FixedPoint& rhs) {
        return lhs.raw_value < rhs.raw_value;
    }
    friend constexpr bool operator>(const FixedPoint& lhs, const FixedPoint& rhs) {
        return lhs.raw_value > rhs.raw_value;
    }
    friend constexpr bool operator<=(const FixedPoint& lhs, const FixedPoint& rhs) {
        return lhs.raw_value <= rhs.raw_value;
    }
    friend constexpr bool operator>=(const FixedPoint& lhs, const FixedPoint& rhs) {
        return lhs.raw_value >= rhs.raw_value;
    }

    // Mathematical functions
    static constexpr FixedPoint Abs(const FixedPoint& v) {
        return FixedPoint(v.raw_value < 0 ? -v.raw_value : v.raw_value);
    }

    static constexpr FixedPoint Min(const FixedPoint& a, const FixedPoint& b) {
        return a < b ? a : b;
    }

    static constexpr FixedPoint Max(const FixedPoint& a, const FixedPoint& b) {
        return a > b ? a : b;
    }

    // Integer square root using Newton-Raphson, no floats.
    // Returns 0 for negative inputs (or you may assert; we choose floor(sqrt(x*SCALE))?).
    // Actually for fixed-point, sqrt(x) * SCALE = sqrt(x_raw * SCALE).
    // So we need sqrt(x_raw * SCALE) in integer.
    static constexpr FixedPoint Sqrt(const FixedPoint& v) {
        if (v.raw_value <= 0) return FixedPoint(0);
        int64_t n = static_cast<int64_t>(v.raw_value) * SCALE; // scale up to get proper magnitude
        if (n <= 1) return FixedPoint(static_cast<int32_t>(n));
        int64_t x = n;
        int64_t y = (x + 1) / 2;
        while (y < x) {
            x = y;
            y = (x + n / x) / 2;
        }
        return FixedPoint(static_cast<int32_t>(x));
    }

    // Distance between two 2D points (both FixedPoint)
    static constexpr FixedPoint Distance2D(const FixedPoint& x1, const FixedPoint& y1,
                                           const FixedPoint& x2, const FixedPoint& y2) {
        FixedPoint dx = x2 - x1;
        FixedPoint dy = y2 - y1;
        // sqrt(dx^2 + dy^2)
        FixedPoint dx2 = dx * dx;
        FixedPoint dy2 = dy * dy;
        return Sqrt(dx2 + dy2);
    }
};

} // namespace moba::math