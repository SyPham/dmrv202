// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

const SYSTEM_CODE = 3;
export const environment = {
  production: true,
  systemCode: SYSTEM_CODE,
  apiUrlEC: 'http://10.4.4.224:1282/api/',
  apiUrl: 'http://10.4.5.174:108/api/',
  apiUrl2: 'http://10.4.5.174:108/api/',
  hub: 'http://10.4.4.224:1282/ec-hub',
  scalingHub: 'http://10.4.4.224:1282/ec-hub',
  scalingHubLocal: 'http://localhost:5001/scalingHub',
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
