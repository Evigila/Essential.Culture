# Use confirmed Trusted Publishing profile

Recorded at 2026-10-06T18:58:55-03:00.

The user confirmed NuGet profile Evigila and reported that the corresponding Trusted Publishing policy was created. Existing workflows had read secrets.NUGET_USER, which is independent of a variable/environment named nuget. Both workflows now supply the confirmed public username directly to NuGet/login@v1, removing the secret-name setup dependency while retaining id-token permission, the nuget environment, tag-only publication and temporary push credentials. The official NuGet/login user input accepts this profile name; no authentication key is stored in source. Successful release runs will establish actual account-policy coverage. Previous setup records remain historical. The user explicitly authorized source commits and publication.
