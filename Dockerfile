FROM ubuntu
COPY ./build/Server ./app
EXPOSE 9150/udp
EXPOSE 9151/tcp
CMD ["app/ubivius-server.x86_64"]
