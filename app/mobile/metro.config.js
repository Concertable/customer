// eslint-disable-next-line @typescript-eslint/no-require-imports
const { getDefaultConfig } = require("expo/metro-config");
// eslint-disable-next-line @typescript-eslint/no-require-imports
const { withNativeWind } = require("nativewind/metro");
// eslint-disable-next-line @typescript-eslint/no-require-imports
const path = require("path");

const config = getDefaultConfig(__dirname);

const customerPackage = path.dirname(require.resolve("@concertable/customer/package.json"));
const mobilePackage = path.dirname(require.resolve("@concertable/mobile/package.json"));
const sharedPackage = path.dirname(require.resolve("@concertable/shared/package.json"));
const mobileNodeModules = path.dirname(
  path.dirname(require.resolve("react-native/package.json", { paths: [mobilePackage] })),
);

config.watchFolders = [customerPackage, mobilePackage, mobileNodeModules, sharedPackage];
config.resolver.nodeModulesPaths = [
  ...(config.resolver.nodeModulesPaths ?? []),
  mobileNodeModules,
];

module.exports = withNativeWind(config, {
  input: require.resolve("@concertable/mobile/global.css"),
  inlineRem: 16,
});
