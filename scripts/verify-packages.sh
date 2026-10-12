#!/usr/bin/env bash
# Verifies the NuGet packages CI and the release workflow build:
#
#   CymruBlazor, CymruBlazor.Testing and CymruBlazor.Testing.Axe
#
# Each must be present as a .nupkg and a .snupkg at the same version (they are released in lock-step from one Git tag
# through MinVer), the two testing packages must depend on exactly that version of the package below them, and the
# files the packages rely on must be inside them.
#
# Usage: bash scripts/verify-packages.sh <folder> [expected-version]
#
# With an expected version (the release workflow passes the tag without its "v") the packages must carry it too.

set -euo pipefail

dir="${1:?Usage: verify-packages.sh <folder> [expected-version]}"
expected="${2:-}"
failed=0

fail() {
  echo "::error::$*"
  failed=1
}

# CymruBlazor.<digit>... only, so CymruBlazor.Testing.* is not matched.
mapfile -t core < <(find "$dir" -maxdepth 1 -type f -name 'CymruBlazor.[0-9]*.nupkg')

if [ "${#core[@]}" -ne 1 ]; then
  fail "Expected exactly 1 CymruBlazor.<version>.nupkg in ${dir}, found ${#core[@]}."
  exit 1
fi

version="$(basename "${core[0]}" .nupkg)"
version="${version#CymruBlazor.}"

echo "Package version: ${version}"

if [ -n "$expected" ] && [ "$version" != "$expected" ]; then
  fail "Package version '${version}' does not match the expected version '${expected}' (the Git tag)."
fi

nupkg_count="$(find "$dir" -maxdepth 1 -type f -name '*.nupkg' | wc -l)"
snupkg_count="$(find "$dir" -maxdepth 1 -type f -name '*.snupkg' | wc -l)"

echo "Found ${nupkg_count} .nupkg and ${snupkg_count} .snupkg file(s)"

if [ "$nupkg_count" -ne 3 ]; then
  fail "Expected exactly 3 .nupkg files, found ${nupkg_count}."
fi

if [ "$snupkg_count" -ne 3 ]; then
  fail "Expected exactly 3 .snupkg files, found ${snupkg_count}."
fi

# contains <package> <entry>: is the file inside the package?
contains() {
  unzip -Z1 "$1" | grep -Fxq "$2"
}

# depends_on <package> <id>: does the nuspec depend on <id> at exactly the package version?
depends_on() {
  unzip -p "$1" '*.nuspec' | grep -Fq "id=\"$2\" version=\"${version}\""
}

for id in CymruBlazor CymruBlazor.Testing CymruBlazor.Testing.Axe; do
  package="${dir}/${id}.${version}.nupkg"
  symbols="${dir}/${id}.${version}.snupkg"

  [ -f "$package" ] || { fail "Missing ${id}.${version}.nupkg."; continue; }
  [ -f "$symbols" ] || fail "Missing ${id}.${version}.snupkg."

  contains "$package" "lib/net10.0/${id}.dll" || fail "${id}.${version}.nupkg has no lib/net10.0/${id}.dll."

  echo "Checked ${id}.${version}"
done

core_package="${dir}/CymruBlazor.${version}.nupkg"
testing_package="${dir}/CymruBlazor.Testing.${version}.nupkg"
axe_package="${dir}/CymruBlazor.Testing.Axe.${version}.nupkg"

# CymruBlazor.Testing.Axe reads the library CSS from the package folder, so the CSS must ship there.
if [ -f "$core_package" ]; then
  contains "$core_package" "staticwebassets/css/cymrublazor.css" \
    || fail "CymruBlazor.${version}.nupkg has no staticwebassets/css/cymrublazor.css; CymruBlazor.Testing.Axe cannot find the library CSS."

  contains "$core_package" "staticwebassets/CymruBlazor.bundle.scp.css" \
    || echo "::warning::CymruBlazor.${version}.nupkg has no staticwebassets/CymruBlazor.bundle.scp.css; scans would miss the per-component styles."
fi

if [ -f "$testing_package" ]; then
  contains "$testing_package" "README.md" || fail "CymruBlazor.Testing.${version}.nupkg has no README.md."
  depends_on "$testing_package" "CymruBlazor" || fail "CymruBlazor.Testing.${version}.nupkg does not depend on CymruBlazor ${version}."
fi

if [ -f "$axe_package" ]; then
  contains "$axe_package" "README.md" || fail "CymruBlazor.Testing.Axe.${version}.nupkg has no README.md."
  depends_on "$axe_package" "CymruBlazor.Testing" || fail "CymruBlazor.Testing.Axe.${version}.nupkg does not depend on CymruBlazor.Testing ${version}."
fi

if [ "$failed" -ne 0 ]; then
  exit 1
fi

echo "All three packages are present at ${version} and consistent."
