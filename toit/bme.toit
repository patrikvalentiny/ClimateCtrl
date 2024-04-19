import gpio
import i2c
import bme280

class BME:
  driver_ /bme280.Driver := ?

  constructor alt-address = false:
    bus := i2c.Bus
      --sda=gpio.Pin 21
      --scl=gpio.Pin 22
    // The BME280 can be configured to have one of two different addresses.
    // - bme280.I2C_ADDRESS, equal to 0x76
    // - bme280.I2C_ADDRESS_ALT, equal to 0x77
    // The address is generally chosen by the break-out board.
    // If the example fails with I2C_READ_FAILED verify that you are using the correct address.
    address := alt-address ? bme280.I2C_ADDRESS_ALT : bme280.I2C_ADDRESS
    device := bus.device address
    driver_ = bme280.Driver device

  get-temp-c -> float:
    return driver_.read_temperature  
  get-temp-f -> float:
    return (driver_.read_temperature * 1.8) + 32
  get-pressure-pa -> float:
    return driver_.read_pressure
  get-humidity-percent -> float:
    return driver_.read_humidity 