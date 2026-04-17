import { mkdir } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { build } from "esbuild";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const extensionRoot = path.resolve(scriptDirectory, "..");
const outputDirectory = path.join(extensionRoot, "dist");

await mkdir(outputDirectory, { recursive: true });

await build({
  entryPoints: [path.join(extensionRoot, "extension.js")],
  bundle: true,
  external: ["vscode"],
  format: "cjs",
  outfile: path.join(outputDirectory, "extension.js"),
  platform: "node",
  sourcemap: true,
  target: "node20"
});
