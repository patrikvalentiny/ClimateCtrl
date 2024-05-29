import .bme
import .utils
import .drv8825

main:
  //init utils
  Utils
  //init motor driver
  driver /StepperDriver ::= DRV8825
  // setup MQTT client  
  bme /BME ::= BME280
  // start BME280 data sending task
  bme.send-bme-data-periodically 1

