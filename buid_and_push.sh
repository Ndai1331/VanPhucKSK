# Build and push WebApi image
echo "Building CoreAdminWeb image for AMD64..."
docker build --platform linux/amd64 -t longnguyen1331/drcore-admin-qlcl-web:latest -f CoreAdminWeb/Dockerfile .
echo "Pushing CoreAdminWeb image..."
docker push longnguyen1331/drcore-admin-qlcl-web:latest
