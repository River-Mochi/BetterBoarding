## Better Boarding

Better Boarding reduces public transport boarding delays without replacing the game's transport AI.

### What It Does

- Faster boarding/loading sliders for bus, rail, ship + ferry, and airplane.
- `Skip Late Passengers`: late solo cims can miss a vehicle after a short grace instead of holding everyone.
- Groups/families are handled together so one straggler does not cause a long delay.
- `Cims Run Sooner`: assigned late passengers start running earlier for bus, tram, train, and subway.
- Compact Options status plus `Stats to Log` for waits, worst stops, skipped passengers, and troubleshooting.

### Safe Design

Better Boarding does not use Harmony and does not replace vanilla transport systems.

- Vanilla still controls routes, departure scheduling, and un-bunching.
- Skipped cims are not deleted; vanilla can reassign or reroute them.
- Group handling works with vanilla boarding/cancellation behavior to keep travelers together.
- Save-game safe and safe to remove anytime.

### Tips

- If a stop has huge waits, use `Stats to Log` and check the worst stops for traffic, bad stop placement, or too few vehicles.
- Keep verbose logging OFF during normal gameplay. It is for testing and can create large log files.

### Compatibility

Use only one boarding-behavior mod at a time. Do not use Better Boarding and All Aboard together.

### Credits

- River-Mochi: mod author
- bcallender's All Aboard and Wayze's InstantBoarding: inspiration
- yenyang: testing and code feedback
- MayorCheeks, Gagaxm, Neco1996, Empiiey: testing
- elGendo87: thumbnail straightener
