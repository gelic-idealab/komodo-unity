# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- New height adjustment mechanism

## [0.2.3] - 2021-03-12
### Changed
- Modularize managers to live independently of each other by using SingletonComponent.cs flag, "is alive" to see if managers exist in scene to run processes connected to managers.
- Separated each manager to their own prefab as well as UIMenu and Avatar.
- UIMenu and Avatar have a new script on root to use for accessing deep references within its hierarchy. The script also allows us to set references when either UIMenu and Avatar appears in the scene instead of having those references from start. This was done so both things can live independently of each other.
- Drawing system now relies on the messaging system.
- Ghost UI cursor now appears on UIMenu from the beginning and reflects the current selection hand.

## [0.2.2] - 2021-02-26
### Added
- The menu will change from being left-handed to being right-handed, depending on which controller you use to activate it. 

### Changed
- Cached models are now detected properly, so they do not get re-downloaded.
- Model button colors are now Komodo Purple.
- The teleport arc is now purple for valid selections and red for invalid selections.

### Fixed
- The model list supports a larger number of items. Previously, the font size was variable, causing the texture to be too large, which resulted in missing letters.
- The multiplayer connection happens later in loading so the connection. Previously, it was possible to join a session where you could not see anyone else but they could see you. (Closes #29)
- 3D models that previously had missing textures now load properly. Internally, we replaced Siccity/GLTFUtility with GLTFast.