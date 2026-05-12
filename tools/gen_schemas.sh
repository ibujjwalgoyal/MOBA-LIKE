#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirma,e "${BASH_SOURCE[0]}")/.." && pwd)"
SCHEMA_DIR="${ROOT_DIR}/shared/schema"
CPP_OUT_DIR="${ROOT_DIR}/gameserver/gen"
CS_OUT_DIR="${ROOT_DIR}/client/Assets/Generated"

mkdir -p "${CPP_OUT_DIR}"
mkdir -p "${CS_OUT_DIR}"

echo "Generating FlatBuffers schemas..."

find "${SCHEMA_DIR}" -name "*.fbs" | while read -r schema
do
    echo "Processing: ${schema}"

    flatc \
        --cpp \
        -o "${CPP_OUT_DIR}" \
        "${schema}"

    flatc \
        --csharp \
        -o "${CS_OUT_DIR}" \
        "${schema}"
done

echo "FlatBuffers generation completed successfully"