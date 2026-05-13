cmake_minimum_required(VERSION 3.22)
project(MOBAGameServer VERSION 0.1.0 LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

option(BUILD_TESTS "Build tests" ON)

if(DEFINED ENV{VCPKG_ROOT} AND NOT DEFINED CMAKE_TOOLCHAIN_FILE)
    set(CMAKE_TOOLCHAIN_FILE "$ENV{VCPKG_ROOT}/scripts/buildsystems/vcpkg.cmake" CACHE STRING "")
endif()

include(FetchContent)

# -------------------------
# Dependencies
# -------------------------

FetchContent_Declare(
    entt
    GIT_REPOSITORY https://github.com/skypjack/entt.git
    GIT_TAG v3.13.2
)

FetchContent_Declare(
    enet
    GIT_REPOSITORY https://github.com/lsalzman/enet.git
    GIT_TAG v1.3.18
)
FetchContent_Declare(
    sol2
    GIT_REPOSITORY https://github.com/ThePhD/sol2.git
    GIT_TAG v3.3.1
)

FetchContent_Declare(
    lua
    URL https://www.lua.org/ftp/lua-5.4.7.tar.gz
)

FetchContent_Declare(
    flatbuffers
    GIT_REPOSITORY https://github.com/google/flatbuffers.git
    GIT_TAG v24.3.25
)

FetchContent_Declare(
    libfixmath
    GIT_REPOSITORY https://github.com/PetteriAimonen/libfixmath.git
    GIT_TAG master
)

FetchContent_Declare(
    spdlog
    GIT_REPOSITORY https://github.com/gabime/spdlog.git
    GIT_TAG v1.14.1
)

FetchContent_Declare(
    catch2
    GIT_REPOSITORY https://github.com/catchorg/Catch2.git
    GIT_TAG v3.5.4
)

FetchContent_MakeAvailable(
    entt
    enet
    sol2
    flatbuffers
    libfixmath
    spdlog
    catch2
)

# -------------------------
# Lua Build
# -------------------------

FetchContent_GetProperties(lua)
if(NOT lua_POPULATED)
    FetchContent_Populate(lua)
    file(GLOB LUA_SOURCES 
        ${lua_SOURCE_DIR}/src/*.c
    )

    list(REMOVE_ITEM LUA_SOURCES
        ${lua_SOURCE_DIR}/src/lua.c
        ${lua_SOURCE_DIR}/src/luac.c
    )

    add_library(lua54 STATIC ${LUA_SOURCES})
    target_include_directories(lua54 PUBLIC
        ${lua_SOURCE_DIR}/src
    )
endif()

# -------------------------
# ENet Build
# -------------------------

file(GLOB ENET_SOURCES
    ${enet_SOURCE_DIR}/*.c
    ${enet_SOURCE_DIR}/unix.c
)

add_library(enet_static STATIC ${ENET_SOURCES})
target_include_directories(enet_static PUBLIC
    ${enet_SOURCE_DIR}/include
)

# -------------------------
# Game Server Sources
# -------------------------

file(GLOB_RECURSE GAME_SERVER_SOURCES
    src/*.cpp
)

add_executable(gameserver
    ${GAME_SERVER_SOURCES}
)

target_include_directories(gameserver PRIVATE
    src
    gen
    ${enet_SOURCE_DIR}/include
    ${lua_SOURCE_DIR}/src
)

target_link_libraries(gameserver PRIVATE
    EnTT::EnTT
    enet_static
    lua54
    sol2
    flatbuffers
    libfixmath
    spdlog::spdlog
)

# -------------------------
# Tests
# -------------------------

if(BUILD_TESTS)

    enable_testing()

    file(GLOB_RECURSE TEST_SOURCES
        tests/*.cpp
    )

    add_executable(gameserver_tests
        ${TEST_SOURCES}
    )

    target_include_directories(gameserver_tests PRIVATE
        src
        gen
        ${lua_SOURCE_DIR}/src
    )

    target_link_libraries(gameserver_tests PRIVATE
        Catch2::Catch2WithMain
        EnTT::EnTT
        enet_static
        lua54
        sol2
        flatbuffers
        libfixmath
        spdlog::spdlog
    )

    include(CTest)
    include(Catch)

    catch_discover_tests(gameserver_tests)

endif()