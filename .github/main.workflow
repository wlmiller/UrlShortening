workflow "Unit test" {
  on = "push"
  resolves = ["Run tests"]
}

action "Run tests" {
  uses = "./"
}
