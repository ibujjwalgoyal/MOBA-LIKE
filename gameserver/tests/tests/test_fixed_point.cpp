#include <catch2/catch_test_macros.hpp>
#include "math/fixed_point.h"

using namespace moba::math;

TEST_CASE("FixedPoint construction", "[fixed_point]") {
    FixedPoint a;
    REQUIRE(a.raw_value == 0);
    FixedPoint b = FixedPoint::FromInt(5);
    REQUIRE(b.raw_value == 5000);
}

TEST_CASE("FixedPoint arithmetic", "[fixed_point]") {
    FixedPoint a = FixedPoint::FromInt(3);   // 3000
    FixedPoint b = FixedPoint::FromInt(2);   // 2000
    FixedPoint c = FixedPoint::FromInt(6);   // 6000

    SECTION("addition") {
        REQUIRE((a + b) == FixedPoint::FromInt(5));
    }
    SECTION("subtraction") {
        REQUIRE((a - b) == FixedPoint::FromInt(1));
    }
    SECTION("multiplication") {
        // 3.0 * 2.0 = 6.0
        REQUIRE((a * b) == FixedPoint::FromInt(6));
    }
    SECTION("division") {
        // 6.0 / 2.0 = 3.0
        REQUIRE((c / b) == FixedPoint::FromInt(3));
    }
    SECTION("negation") {
        REQUIRE((-a) == FixedPoint::FromInt(-3));
    }
}

TEST_CASE("FixedPoint comparison", "[fixed_point]") {
    FixedPoint a = FixedPoint::FromInt(5);
    FixedPoint b = FixedPoint::FromInt(5);
    FixedPoint c = FixedPoint::FromInt(6);

    REQUIRE(a == b);
    REQUIRE(a != c);
    REQUIRE(a < c);
    REQUIRE(c > a);
    REQUIRE(a <= b);
    REQUIRE(c >= a);
}

TEST_CASE("FixedPoint sqrt", "[fixed_point]") {
    SECTION("sqrt of 4") {
        FixedPoint v = FixedPoint::FromInt(4); // 4000
        FixedPoint s = FixedPoint::Sqrt(v);
        // sqrt(4) = 2, raw = 2000
        REQUIRE(s.raw_value == 2000);
    }
    SECTION("sqrt of 2") {
        FixedPoint v = FixedPoint::FromInt(2); // 2000
        FixedPoint s = FixedPoint::Sqrt(v);
        // sqrt(2) ≈ 1.41421356 => raw = 1414 (floor)
        REQUIRE(s.raw_value == 1414);
    }
    SECTION("sqrt of 0") {
        REQUIRE(FixedPoint::Sqrt(FixedPoint()).raw_value == 0);
    }
    SECTION("sqrt of negative returns 0") {
        FixedPoint neg = FixedPoint::FromInt(-1);
        REQUIRE(FixedPoint::Sqrt(neg).raw_value == 0);
    }
}

TEST_CASE("FixedPoint Distance2D", "[fixed_point]") {
    // points (0,0) to (3,4) => distance 5.0
    FixedPoint zero = FixedPoint();
    FixedPoint x2 = FixedPoint::FromInt(3);
    FixedPoint y2 = FixedPoint::FromInt(4);
    FixedPoint dist = FixedPoint::Distance2D(zero, zero, x2, y2);
    REQUIRE(dist.raw_value == 5000); // 5.0 * 1000 = 5000
}

TEST_CASE("FixedPoint overflow handling", "[fixed_point]") {
    FixedPoint max = FixedPoint::FromRaw(2000000000); // near 2e9
    FixedPoint two = FixedPoint::FromInt(2);
    // multiplication should not crash, but result may be clamped/wrapped; check that it uses int64
    auto prod = max * two;
    REQUIRE(prod.raw_value >= 0); // just ensure no crash, specifics depend on intermediate
    // division safe
    auto div = max / two;
    REQUIRE(div.raw_value == 1000000000);
}