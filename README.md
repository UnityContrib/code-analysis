# code-analysis

Roslyn code analyzers

## What does it do?

Basicly it generates compiler warnings and errors specific to Unity code.

The idea is to have a few rules that ensures a good level of quality code. E.g. if a field is visible in the Unity inspector, it must have a tooltip attribute describing what it does, what it is used for or allowed values.

## Current Rules

| ID | Description | Has Code Fixer |
| :---: | --- | :---: |
| HasToolTip | Requires private instance fields marked with [[SerializeField](http://docs.unity3d.com/ScriptReference/SerializeField.html)] attribute to also have a [[ToolTip](http://docs.unity3d.com/ScriptReference/TooltipAttribute.html)] attribute when the class derrives from [MonoBehaviour](http://docs.unity3d.com/ScriptReference/MonoBehaviour.html). | YES |

## Future Rules

| ID | Description |
| :---: | --- | :---: |
| NoPublicFields | Public instance fields should be made private when the class derrives from [MonoBehaviour](http://docs.unity3d.com/ScriptReference/MonoBehaviour.html). A property can be used to access the state outside the class. |
| NoEmptyTooltips | Tooltip attributes on field members on MonoBehaviours may not be empty. |

## Current state

OK

- Only one rule
