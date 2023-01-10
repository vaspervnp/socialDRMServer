# socialDRMServer
API Server (.Net 7) for modifying epub files to include social DRM

1st working version that supports 2 calls, AddSocialDrm and GetBookTitle

AddSocialDrm example:
curl --location --request POST 'https://localhost:7038/Epub/AddSocialDrm' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'epubSource=<base64 string of epub>' \
--data-urlencode 'socialName=<name to put as socialdrm>'

Returns a base64 representation of the epub file containing the social drm.


GetBookTitle example:
curl --location --request POST 'https://localhost:7038/Epub/GetBookTitle' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'epubSource=<base64 string of epub>'

Returns the title of the book.
