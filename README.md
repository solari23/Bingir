# Bingir: The Bing Image Retriever
A lightweight CLI utility that can fetch and locally cache Bing's image-of-the-day.

The location of the cache on disk as well as the max number of images to cache is configurable.

## Usage examples
#### Fetch and cache today's image
`Binger.exe fetch`

#### Fetch the last 3 days' images
`Binger.exe fetch -n 3`

#### Return the local filepath to the latest cached image
`Binger.exe latest`

#### Refresh the cache with the latest image and return its local filepath
`Binger.exe latest -f`
