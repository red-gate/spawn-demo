FROM gitpod/workspace-full
RUN curl https://run.spawn.cc/install | sh
ENV PATH="${HOME}/.spawnctl/bin:${PATH}"
RUN curl -L -o dotnetinstall.sh https://dot.net/v1/dotnet-install.sh && chmod +x dotnetinstall.sh && ./dotnetinstall.sh --channel 3.1
ENV PATH="${HOME}/.dotnet:${PATH}"