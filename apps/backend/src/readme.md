# docker server web socket

This is a python file with web socket implementation which runs on localhost: 8765.

** **Make sure that python is installed and docker is running locally before running this python file.** **

It consists the implementation for 
- creating containers.
- pulling the images.
- start and stop containers.
- getting the stats of the container.
- list of containers.
- list of docker images.



### Requesting the server for the data.

 using the c# client, requests can be send to the server to fetch the data.
  - List docker images
    ```
    ws.send("list_images")
    ```
- delete image
    ```
        ws.send("remove_image:image_name")
    ```
 - List docker containers
    ```
    ws.send("list_containers")
    ```
 - To pull docker image
    ```
    ws.send("pull_image:image_name")
    ```

- To create containers
    ```
    ws.send("create_container:image_name")
    ```
    **example:-** uzeyexe/tetris

- Get the container stats
    ```
     ws.send("stats:container_name")
    ```
     **example:-** tetris_container.
- Start container
    ```
        ws.send("start_container:container_name")
    ```
    **example:-** tetris_container.

- Stop container
    ```
        ws.send("stop_container:container_name")
    ```
    **example:-** tetris_container.

- delete container
    ```
        ws.send("remove_container:container_name")
    ```

:EMOJICODE: grinning:







