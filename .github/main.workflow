workflow "Unit test" {
  on = "push"
  resolves = ["Run tests"]
}

action "Run tests" {
  uses = "docker://microsoft/dotnet:2.0-sdk"
  runs = "echo TESTING"
}
