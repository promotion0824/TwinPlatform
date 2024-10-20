{
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/master";
    flake-utils.url = "github:numtide/flake-utils/master";
  };
  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
      in
      {
        devShells.default = pkgs.mkShell {
          nativeBuildInputs = [
            pkgs.nodejs-16_x-openssl_1_1
            pkgs.rnix-lsp
          ];
        };
      });
}
