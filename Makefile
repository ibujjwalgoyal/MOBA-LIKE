SHELL := /bin/bash
ROOT_DIR := $(shell pwd)
GAME_SERVER_DIR := $(ROOT_DIR)/gameserver
BACKEND_DIR := $(ROOT_DIR)/backend
TOOLS_DIR := $(ROOT_DIR)/tools

.PHONY: build-all test-all gen-schemas build-gameserver build-backend test-gameserver test-backend clean
build-all : gen-schemas build-gameserver build-backend
test-all : test-gameserver test-backend

gen-schemas:
	chmod +x $(TOOLS_DIR)/gen_schemas.sh
	$(TOOLS_DIR)/gen_schemas.sh


build-gameserver:
	cmake -S $(GAME_SERVER_DIR) -B $(GAME_SERVER_DIR)/build -DCMAKE_BUILD_TYPE=Release
	cmake --build $(GAME_SERVER_DIR)/build -j

build-backend:
	cd $(BACKEND_DIR) && dotnet build MOBA.sln --configuration Release

test-gameserver:
	ctest --test-dir $(GAME_SERVER_DIR)/build --output-on-failure

test-backend:
	cd $(BACKEND_DIR) && dotnet test MOBA.sln --configuration Release

clean:
	rm -rf $(GAME_SERVER_DIR)/build
	rm -rf $(GAME_SERVER_DIR)/gen
	rm -rf $(ROOT_DIR)/client/Assets/Generated