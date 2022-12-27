# dotnet_gitlab_code_quality

## What does it do?

Gitlabs Code Quality Issue format is different from the format used by .Net to report code quality issues (Sarif 1.0 as of the time of writing). Reporting code quality issues in Gitlab is therefore not really possible.
This tool aims to rectify this problem by offering three functions:

- Convert Microsoft Build time code quality issues into Gitlabs format
- Convert Roslynator issues into Gitlabs format
- Merge multiple Gitlab files into one file

### Example:

I assume that you have your Project at c:\dev\myproject and you have build it, so that a codequality file exists at `c:\dev\myproject\codeanalysis.sarif.json`

Now we want to generate a Gitlab compatible file:
```shell
dotnet tool run cq sarif codeanalysis.sarif.json targetfile.json c:/dev
```

For Roslynator:
```shell
dotnet tool run cq roslynator roslynator.xml targetfile.json c:/dev
```

For merging:

```shell
dotnet tool run cq merge target.json source1.json source2.json
```

Note the third argument, it is used to report only the path relative to the repository, not the full local path.
Now you can upload your file in Gitlab und you SHOULD be able to see it in the merge view 
Gotcha: Gitlab compares issues to the target of the merge. When there are no issues in the target branch, it will not display anything. So please run this tool on your main branch first then open a merge request to see it in the Gitlab UI.

Gitlab Pipeline should look like this:

```yaml
code_quality_job:
  image: mcr.microsoft.com/dotnet/sdk:7.0
  stage: test
  script:
    - 'dotnet build ./MySln.sln'
    - 'dotnet tool run roslynator analyze ./MySln.sln -o roslynator.xml  || true' 
    - 'dotnet tool run cq roslynator.xml gl-code-quality-report.json c:\dev'
  artifacts:
    paths:
      - roslynator.xml
      - gl-code-quality-report.json
    expose_as: 'code_quality_reports'
    reports:
      codequality: gl-code-quality-report.json
  
  rules:
    - if: $CI_MERGE_REQUEST_ID
    - if: $CI_COMMIT_REF_NAME == "release"
    - if: $CI_COMMIT_REF_NAME == "develop"
  when:
    always
  allow_failure: false
```

## How to install?

## How to contribute?

Convert .Net code quality reports into gitlab code quality reports


