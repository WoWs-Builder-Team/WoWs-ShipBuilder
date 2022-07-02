FROM nginx

COPY ./Deployment/Local/ssl-config.nginx /etc/nginx/snippets/ssl-config.conf
COPY ./Deployment/Local/nginx-testing.crt /etc/ssl/certs/nginx-testing.crt
COPY ./Deployment/Local/nginx-testing.key /etc/ssl/certs/nginx-testing.key
COPY ./Deployment/Local/nginx.local.nginx /etc/nginx/conf.d/default.conf

#RUN sudo apt-add-repository -y ppa:hda-me/nginx-stable
#RUN sudo apt-get update
#RUN sudo apt-get install brotli nginx nginx-module-brotli

COPY ./bin/Release/net6.0/publish/wwwroot /usr/share/nginx/html
