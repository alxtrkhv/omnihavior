set unstable
set script-interpreter := ['uv', 'run', '--script']

[private]
default:
  @just --list

test:
  dotnet test Omnihavior.Tests/Omnihavior.Tests.csproj

build: test
  dotnet build --configuration release Omnihavior/Omnihavior.csproj

[script]
set-version new_version:
    import re
    import subprocess
    import sys
    from pathlib import Path

    new_version = "{{ new_version }}"
    csproj_path = Path("Omnihavior/Omnihavior.csproj")
    package_json_path = Path("Omnihavior.Unity/package.json")
    tag_name = f"release/v{new_version}"
    commit_message = f"release: v{new_version}"

    print("Checking git status...")
    try:
        status_result = subprocess.run(["git", "status", "--porcelain"], check=True, capture_output=True, text=True)
        if status_result.stdout:
            print("Error: Git working directory is not clean. Please commit or stash changes.")
            print(status_result.stdout)
            sys.exit(1)
        print("Git status is clean.")

        print(f"Setting version to {new_version} in {csproj_path}...")
        content = csproj_path.read_text()
        updated_content, count = re.subn(
            r"(<Version>)(.*?)(</Version>)",
            rf"\g<1>{new_version}\g<3>",
            content,
            count=1
        )

        if count == 0:
            print(f"Error: Could not find <Version> tag in {csproj_path}")
            sys.exit(1)

        csproj_path.write_text(updated_content)
        print(f"Successfully updated {csproj_path}")

        print(f"Setting version to {new_version} in {package_json_path}...")
        package_content = package_json_path.read_text()
        updated_package_content, package_count = re.subn(
            r'("version":\s*")(.*?)(")',
            rf'\g<1>{new_version}\g<3>',
            package_content,
            count=1
        )

        if package_count == 0:
            print(f"Error: Could not find version field in {package_json_path}")
            sys.exit(1)

        package_json_path.write_text(updated_package_content)
        print(f"Successfully updated {package_json_path}")

        print(f"Staging {csproj_path} and {package_json_path}...")
        subprocess.run(["git", "add", str(csproj_path), str(package_json_path)], check=True)
        print(f"Successfully staged {csproj_path} and {package_json_path}")

        print(f"Creating git commit with message '{commit_message}'...")
        subprocess.run(["git", "commit", "-m", commit_message], check=True, capture_output=True, text=True)
        print("Successfully created git commit.")

        print(f"Creating git tag {tag_name}...")
        subprocess.run(["git", "tag", tag_name], check=True, capture_output=True, text=True)
        print(f"Successfully created git tag {tag_name}")

    except FileNotFoundError as e:
        if "Omnihavior.csproj" in str(e):
            print(f"Error: {csproj_path} not found.")
        elif "package.json" in str(e):
            print(f"Error: {package_json_path} not found.")
        else:
            print(f"Error: File not found - {e}")
        sys.exit(1)
    except subprocess.CalledProcessError as e:
        command = " ".join(e.cmd)
        print(f"Error executing git command '{command}':")
        print(e.stderr)
        sys.exit(1)
    except subprocess.CalledProcessError as e:
        print(f"Error creating git tag {tag_name}:")
        print(e.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
        sys.exit(1)

