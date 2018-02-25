### Running the benchmars
1. Generate the imput images:
  - Run `Generator.ps1`
  - ImageMagick is a prerequisite
2. Build and run the project, it will execute in interactive mode, with default parameters
3. Run the `.bat` files to execute pre-defined configurations (described below)

### Benchmark scenario
- A fake, in process "web service" is being stressed by load+resize+save requests
- The "client" is "sending" input images of random sizes. The height is always `width/1.77` the width follows a customizable a logonormal distribution like this:
![lognormal](https://i.imgur.com/rM0sjtE.png)
- The time between requests is following a customizable [exponential distribution](https://en.wikipedia.org/wiki/Exponential_distribution)
- The data series follow 610 requests for each setup. The memory data usage is sampled each 50th request
  - Working Set of the process
  - Allocated GC memory
- The raw throughput metric is `ms/MP`: milliseconds needed to process one MegaPixel of input stuff. The lower is better.
- All setups are launched with the same random seed, making the comparison quite fair.
- It's clear from the results, that around 200th request a big outlier image is being processed, consuming ~130MB of memory

### Benchmark configurations
1. `3200p-600ms`: Mid-resolution images, low pressure
    - Mean image width: 3200 pixels, deviation: 1000
    - Mean time between requests: 600ms
    - Requests are likely to be processed in parallel, but not always
2. `3200p-200ms`: Mid-sized images, high pressure
    - Mean image width: 3200 pixels, deviation: 1000
    - Mean time between requests: 200ms
        - Requests are almost always processed in parallel
3. `4000p-1000ms`: High-resolution images, low pressure
    - Mean image width: 4000 pixels, deviation: 1500
     - Images of width 8000p-10000p are not uncommon!
    - Mean time between requests: 1000ms
    - Requests are likely to be processed in parallel, but not always
4. `4000p-300ms`: High-resolution images, high pressure
    - Mean image width: 3200 pixels, deviation: 1000
  	  - Images of width 8000p-10000p are not uncommon!
    - Mean time between requests: 200ms
      - Requests are almost always processed in parallel

### Results
- [Link to the result data](https://1drv.ms/x/s!AkF1IWe7aWXrm17hrGM1htnxT1if)
- Configuration used: CPU: Core i7-770HQ + 16GB RAM

### Conclusions:
TODO
