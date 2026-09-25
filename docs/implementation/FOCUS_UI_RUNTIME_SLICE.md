# Focus UI and runtime integration slice

The Foundation shell now opens First Launch on a new save, then the placeholder
Town and Focus Setup. Continue stores the onboarding decision. Focus Setup lists
optional bare process names, 25/45/60 minutes, and equal-reward display choices.
Ghost is visibly unavailable until exported Windows click-through certification.

`PrototypeRuntime` composes the existing FocusSessionCoordinator, TownSimulation,
JSON store and lifecycle scheduler. The Godot controller sends commands, binds
immutable snapshots, and runs one logical timer. Companion/Hidden changes never
enter the session or Energy calculation. Each completion/stop is committed
before a passive Tray status change, and the Town is not forced open. Tray Open
shows the current world; End Focus and Quit are explicit. A quit while Focus is
running asks for confirmation and saves counted time before exit.

On restart, a saved active checkpoint displays Resume or End at saved time;
time away is excluded. The integration tests cover 25 Focus with mode changes,
Workshop completion/reveal after restart, and recovery after a long shutdown.
The stage's temporary geometry and Town layout still require visual review.

Current limits: the menu's Ghost command remains disabled, the notification is
Tray status only pending Windows toast validation, and the Reward Reveal scene
is not connected yet. Native window, process-picker, tray and sleep/lock checks
remain exported Windows gates. F8 and F10 are opt-in visual QA shortcuts when no
session is active; neither changes gameplay or records progress.
