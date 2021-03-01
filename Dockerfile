FROM ubuntu
COPY ./build/Server ./app
CMD ["app/ubivius-server.x86_64"]
